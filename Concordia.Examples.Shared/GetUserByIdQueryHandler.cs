// Example query handler for user permission management
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Handles <see cref="GetUserByIdQuery"/> by returning the user and their permissions.
    /// </summary>
    /// <seealso cref="IRequestHandler{GetUserByIdQuery, UserDto}"/>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the user dto</returns>
        public Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Getting user: Id='{request.UserId}'");
            var user = new UserDto
            {
                UserId = request.UserId,
                Name = $"User {request.UserId}",
                Permissions = new List<string> { "products:read" }
            };
            return Task.FromResult(user);
        }
    }
}
