// Example Controller for usage (remains unchanged)
using System.Threading;
using System.Threading.Tasks;

namespace Synaptrix.Examples.Web.Controllers
{
    /// <summary>
    /// The hello command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{HelloCommand, string}"/>
    public class HelloCommandHandler : IRequestHandler<HelloCommand, string>
    {
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the string</returns>
        public ValueTask<string> Handle(HelloCommand request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>($"Hello, {request.Name}!");
        }
    }
}